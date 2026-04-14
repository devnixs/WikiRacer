#!/usr/bin/env python3
"""Generate curated French Wikipedia article seeds for WikiRacer.

The generator samples random main-namespace articles from fr.wikipedia, asks a
local LLM to estimate whether each title is broadly recognizable, and keeps only
articles whose score reaches the configured threshold.
"""

from __future__ import annotations

import argparse
import datetime as dt
import json
import random
import re
import time
import urllib.error
import urllib.parse
import urllib.request
from pathlib import Path


USER_AGENT = "WikiRacer curated article generator (https://github.com/devnixs/WikiRacer)"
WIKI_API = "https://fr.wikipedia.org/w/api.php"
PAGEVIEW_BASE = "https://wikimedia.org/api/rest_v1/metrics/pageviews/top/fr.wikipedia/all-access"
DEFAULT_LLM_URL = "http://localhost:1234/api/v1/chat"
DEFAULT_LLM_MODEL = "qwen/qwen3.5-9b"

LLM_SYSTEM_PROMPT = (
    "Estime la probabilité entre 1 et 100 qui indique à quel point une personne "
    "au hasard connaisse l'élément suivant Par exemple, 'Terre' (la planète) "
    "serait un 100 et 'Pieter Jansz Saenredam' serait un 0. Répond uniquement "
    "avec le nombre entre O et 100 et rien d'autre."
)

PROJECT_ROOT = Path(__file__).resolve().parents[1]
SELECTOR_PATH = PROJECT_ROOT / "backend/src/WikiRacer.Infrastructure/Articles/CuratedPlayableArticleSelector.cs"

FR_POOL_BEGIN = "            [\"fr\"] =\n            [\n"
FR_POOL_END = "            ]\n"


def request_json(url: str, *, timeout: int = 30) -> dict:
    request = urllib.request.Request(url, headers={"User-Agent": USER_AGENT})

    for attempt in range(3):
        try:
            with urllib.request.urlopen(request, timeout=timeout) as response:
                return json.loads(response.read().decode("utf-8"))
        except urllib.error.HTTPError:
            if attempt == 2:
                raise
            time.sleep(0.75 * (attempt + 1))

    raise RuntimeError(f"Could not fetch {url}")


def post_json(url: str, payload: dict, *, timeout: int = 60) -> dict:
    body = json.dumps(payload).encode("utf-8")
    request = urllib.request.Request(
        url,
        data=body,
        headers={
            "Content-Type": "application/json",
            "User-Agent": USER_AGENT,
        },
        method="POST")

    for attempt in range(3):
        try:
            with urllib.request.urlopen(request, timeout=timeout) as response:
                return json.loads(response.read().decode("utf-8"))
        except urllib.error.HTTPError:
            if attempt == 2:
                raise
            time.sleep(0.75 * (attempt + 1))

    raise RuntimeError(f"Could not post to {url}")


def random_article_titles(batch_size: int) -> list[str]:
    query = urllib.parse.urlencode({
        "action": "query",
        "format": "json",
        "generator": "random",
        "grnnamespace": "0",
        "grnlimit": str(batch_size),
        "prop": "info",
        "redirects": "1",
    })
    payload = request_json(f"{WIKI_API}?{query}")
    pages = payload.get("query", {}).get("pages", {}).values()
    titles = [
        page["title"]
        for page in pages
        if page.get("ns") == 0 and "missing" not in page and looks_playable_title(page.get("title", ""))
    ]

    return sorted(set(titles), key=str.casefold)


def random_recently_viewed_titles(batch_size: int, pool_size: int, days_back: int, target_candidates: int) -> list[str]:
    pool = pageview_candidate_pool(pool_size, days_back, target_candidates)
    random.shuffle(pool)
    return pool[:batch_size]


def pageview_candidate_pool(pool_size: int, days_back: int, target_candidates: int) -> list[str]:
    pool: list[str] = []
    seen: set[str] = set()
    today = dt.datetime.now(dt.UTC).date()

    for offset in range(1, days_back + 1):
        day = today - dt.timedelta(days=offset)
        url = f"{PAGEVIEW_BASE}/{day:%Y/%m/%d}"

        try:
            payload = request_json(url)
        except urllib.error.HTTPError:
            continue

        articles = payload.get("items", [{}])[0].get("articles", [])

        for article in articles[:pool_size]:
            title = urllib.parse.unquote(article.get("article", "")).replace("_", " ")
            normalized = title.casefold()

            if normalized not in seen and looks_playable_title(title):
                seen.add(normalized)
                pool.append(title)

        if offset == 1 or offset % 10 == 0 or len(pool) >= target_candidates:
            print(f"Loaded {len(pool)} distinct pageview candidates after {offset} day(s).", flush=True)

        if len(pool) >= target_candidates:
            break

    return sorted(pool, key=str.casefold)


def looks_playable_title(title: str) -> bool:
    lower = title.lower()

    if not title or title == "-":
        return False

    if ":" in title:
        return False

    blocked_prefixes = (
        "wikipédia",
        "discussion",
        "utilisateur",
        "modèle",
        "fichier",
        "catégorie",
        "portail",
        "aide",
        "spécial",
        "mediawiki",
    )

    if lower.startswith(blocked_prefixes):
        return False

    blocked_fragments = (
        "liste des",
        "liste de",
        "homonymie",
        "chronologie",
        "discographie",
        "filmographie",
        "bibliographie",
        "résultats détaillés",
        "statistiques et records",
        "saison ",
        "épisode ",
        "décès en ",
        "naissance en ",
        "pornographie",
        "pornographique",
    )

    return not any(fragment in lower for fragment in blocked_fragments)


def score_title(title: str, llm_url: str, model: str, score_cache: dict[str, int]) -> int | None:
    cache_key = title.casefold()

    if cache_key in score_cache:
        return score_cache[cache_key]

    payload = {
        "model": model,
        "system_prompt": LLM_SYSTEM_PROMPT,
        "input": title,
        "stream": False,
        "reasoning": "off",
        "context_length": 1024,
        "store": False,
        "temperature": 0.1,
    }
    response = post_json(llm_url, payload)
    output = response.get("output", [])

    if not output:
        return None

    content = str(output[0].get("content", "")).strip()
    match = re.search(r"\d+", content)

    if match is None:
        return None

    score = int(match.group(0))
    score_cache[cache_key] = max(0, min(score, 100))
    return score_cache[cache_key]


def read_score_cache(path: Path | None) -> dict[str, int]:
    if path is None or not path.exists():
        return {}

    payload = json.loads(path.read_text(encoding="utf-8"))
    return {str(key): int(value) for key, value in payload.items()}


def write_score_cache(path: Path | None, score_cache: dict[str, int]) -> None:
    if path is None:
        return

    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(score_cache, ensure_ascii=False, indent=2, sort_keys=True), encoding="utf-8")


def collect_titles(
    count: int,
    threshold: int,
    source: str,
    batch_size: int,
    pageview_pool_size: int,
    pageview_days_back: int,
    pageview_target_candidates: int,
    llm_url: str,
    model: str,
    sleep_seconds: float,
    max_attempts: int,
    score_cache_path: Path | None) -> list[str]:
    selected: list[str] = []
    seen: set[str] = set()
    score_cache = read_score_cache(score_cache_path)
    attempts = 0

    if source == "pageviews":
        candidates = pageview_candidate_pool(pageview_pool_size, pageview_days_back, pageview_target_candidates)
        random.shuffle(candidates)

        for title in candidates:
            normalized = title.casefold()

            if normalized in seen:
                continue

            seen.add(normalized)
            score = score_title(title, llm_url, model, score_cache)
            accepted = score is not None and score >= threshold
            marker = "keep" if accepted else "skip"
            print(f"[{len(selected):04d}/{count}] {marker:4} {score!s:>3} {title}", flush=True)

            if accepted:
                selected.append(title)

                if len(selected) >= count:
                    write_score_cache(score_cache_path, score_cache)
                    return selected

            if len(seen) % 100 == 0:
                write_score_cache(score_cache_path, score_cache)

            if sleep_seconds > 0:
                time.sleep(sleep_seconds)

        write_score_cache(score_cache_path, score_cache)
        raise SystemExit(
            f"Only generated {len(selected)} valid titles from {len(candidates)} pageview candidates; requested {count}.")

    while len(selected) < count and attempts < max_attempts:
        attempts += 1

        candidates = random_article_titles(batch_size)

        for title in candidates:
            normalized = title.casefold()

            if normalized in seen:
                continue

            seen.add(normalized)
            score = score_title(title, llm_url, model, score_cache)
            accepted = score is not None and score >= threshold
            marker = "keep" if accepted else "skip"
            print(f"[{len(selected):04d}/{count}] {marker:4} {score!s:>3} {title}", flush=True)

            if accepted:
                selected.append(title)

                if len(selected) >= count:
                    write_score_cache(score_cache_path, score_cache)
                    break

            if len(seen) % 100 == 0:
                write_score_cache(score_cache_path, score_cache)

            if sleep_seconds > 0:
                time.sleep(sleep_seconds)

    if len(selected) < count:
        write_score_cache(score_cache_path, score_cache)
        raise SystemExit(
            f"Only generated {len(selected)} valid titles after {attempts} batches; requested {count}.")

    return selected


def csharp_string(value: str) -> str:
    return value.replace("\\", "\\\\").replace("\"", "\\\"")


def render_entries(titles: list[str]) -> str:
    lines: list[str] = []

    for index, title in enumerate(titles):
        comma = "," if index < len(titles) - 1 else ""
        lines.append(f"                new(\"{csharp_string(title)}\", 10, 9, 10){comma}")

    return "\n".join(lines)


def replace_fr_pool(source: str, entries: str) -> str:
    start = source.index(FR_POOL_BEGIN) + len(FR_POOL_BEGIN)
    end = source.index(FR_POOL_END, start)
    return source[:start] + entries + "\n" + source[end:]


def validate_count(source: str) -> int:
    start = source.index(FR_POOL_BEGIN) + len(FR_POOL_BEGIN)
    end = source.index(FR_POOL_END, start)
    return len(re.findall(r"new\(\"", source[start:end]))


def main() -> None:
    parser = argparse.ArgumentParser(description="Generate curated fr.wikipedia article seeds from random pages.")
    parser.add_argument("--count", type=int, default=50, help="Number of French entries to generate.")
    parser.add_argument("--threshold", type=int, default=95, help="Minimum LLM recognizability score to keep a title.")
    parser.add_argument(
        "--source",
        choices=("pageviews", "wikipedia-random"),
        default="pageviews",
        help="Sample randomly from recent pageviews by default, or from all Wikipedia main-namespace pages.")
    parser.add_argument("--batch-size", type=int, default=300, help="Random candidate titles fetched per batch.")
    parser.add_argument("--pageview-pool-size", type=int, default=3000, help="Top viewed pages considered before shuffling.")
    parser.add_argument("--pageview-days-back", type=int, default=14, help="Recent days tried when reading pageview snapshots.")
    parser.add_argument("--pageview-target-candidates", type=int, default=10000, help="Stop loading pageviews after this many distinct candidates.")
    parser.add_argument("--llm-url", default=DEFAULT_LLM_URL, help="Local LLM chat endpoint.")
    parser.add_argument("--model", default=DEFAULT_LLM_MODEL, help="Local LLM model name.")
    parser.add_argument("--sleep", type=float, default=0.0, help="Delay between LLM calls.")
    parser.add_argument("--max-attempts", type=int, default=200, help="Maximum random Wikipedia batches to inspect.")
    parser.add_argument("--score-cache", type=Path, help="Optional JSON score cache for long generation runs.")
    parser.add_argument("--write", action="store_true", help="Write entries into CuratedPlayableArticleSelector.cs.")
    args = parser.parse_args()

    selected = collect_titles(
        count=args.count,
        threshold=args.threshold,
        source=args.source,
        batch_size=args.batch_size,
        pageview_pool_size=args.pageview_pool_size,
        pageview_days_back=args.pageview_days_back,
        pageview_target_candidates=args.pageview_target_candidates,
        llm_url=args.llm_url,
        model=args.model,
        sleep_seconds=args.sleep,
        max_attempts=args.max_attempts,
        score_cache_path=args.score_cache)
    entries = render_entries(selected)

    if args.write:
        source = SELECTOR_PATH.read_text(encoding="utf-8")
        updated = replace_fr_pool(source, entries)
        SELECTOR_PATH.write_text(updated, encoding="utf-8", newline="\n")
        actual_count = validate_count(updated)

        if actual_count != args.count:
            raise SystemExit(f"Wrote {actual_count} French entries; expected {args.count}.")

        print(f"Wrote {actual_count} French entries to {SELECTOR_PATH}")
    else:
        print(entries)


if __name__ == "__main__":
    main()
