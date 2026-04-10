import { inject, Injectable } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { ArticleRenderResponse } from './data/article-render.models';

export interface TocEntry {
  readonly id: string;
  readonly level: number;
  readonly title: string;
}

export interface PreparedArticleDocument {
  readonly html: SafeHtml;
  readonly toc: readonly TocEntry[];
}

@Injectable({ providedIn: 'root' })
export class ArticleHtmlSanitizerService {
  private readonly sanitizer = inject(DomSanitizer);

  prepare(document: ArticleRenderResponse, publicLobbyId: string): PreparedArticleDocument {
    const parsed = new DOMParser().parseFromString(document.html, 'text/html');
    const container = parsed.body ?? parsed.documentElement;

    container.querySelectorAll(
      'script,style,link,meta,base,iframe,frame,object,embed,form,input,button,textarea,select,noscript,svg,math,img,figure'
    ).forEach((node) => node.remove());

    container.querySelectorAll('*').forEach((element) => {
      for (const attribute of Array.from(element.attributes)) {
        const name = attribute.name.toLowerCase();

        if (
          name.startsWith('on') ||
          name === 'style' ||
          name === 'id' ||
          name === 'class' ||
          name === 'about' ||
          name === 'typeof' ||
          name === 'resource' ||
          name === 'prefix' ||
          name.startsWith('data-mw') ||
          name.startsWith('data-parsoid')
        ) {
          element.removeAttribute(attribute.name);
        }
      }
    });

    // Extract TOC after attribute cleanup, and assign IDs to headings for anchor navigation
    const toc: TocEntry[] = [];
    let headingIndex = 0;

    container.querySelectorAll('h2, h3').forEach((heading) => {
      const title = (heading.textContent ?? '').trim();

      if (!title) {
        return;
      }

      const id = `toc-${headingIndex++}`;
      heading.setAttribute('id', id);
      toc.push({ id, level: heading.tagName === 'H2' ? 2 : 3, title });
    });

    container.querySelectorAll('a[href]').forEach((anchor) => {
      const href = anchor.getAttribute('href');

      if (!href) {
        return;
      }

      const resolved = this.resolveHref(href, document.sourceUrl);

      if (resolved && this.isInternalArticleLink(resolved, document.language)) {
        const articleTitle = this.toTitleFromUrl(resolved);

        if (!articleTitle) {
          anchor.removeAttribute('href');
          return;
        }

        anchor.setAttribute('href', `/match/${publicLobbyId}?article=${encodeURIComponent(articleTitle)}`);
        anchor.setAttribute('data-internal-link', articleTitle);
        anchor.setAttribute('rel', 'nofollow');
        anchor.removeAttribute('target');
        return;
      }

      anchor.setAttribute('target', '_blank');
      anchor.setAttribute('rel', 'noopener noreferrer external nofollow');
    });

    return {
      html: this.sanitizer.bypassSecurityTrustHtml(container.innerHTML),
      toc
    };
  }

  private resolveHref(href: string, sourceUrl: string): URL | null {
    try {
      return new URL(href, sourceUrl);
    } catch {
      return null;
    }
  }

  private isInternalArticleLink(url: URL, language: string): boolean {
    return url.hostname === `${language}.wikipedia.org`
      && url.pathname.startsWith('/wiki/')
      && !decodeURIComponent(url.pathname.slice('/wiki/'.length)).includes(':');
  }

  private toTitleFromUrl(url: URL): string | null {
    const path = url.pathname.slice('/wiki/'.length);

    if (!path) {
      return null;
    }

    return decodeURIComponent(path).replaceAll('_', ' ');
  }
}
