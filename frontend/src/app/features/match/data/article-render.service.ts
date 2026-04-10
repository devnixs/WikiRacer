import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ArticleRenderResponse } from './article-render.models';

@Injectable({ providedIn: 'root' })
export class ArticleRenderService {
  private readonly httpClient = inject(HttpClient);

  async render(language: string, title: string): Promise<ArticleRenderResponse> {
    const params = new HttpParams()
      .set('language', language)
      .set('title', title);

    return firstValueFrom(this.httpClient.get<ArticleRenderResponse>('/api/articles/render', { params }));
  }
}
