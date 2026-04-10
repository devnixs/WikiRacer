import { Component, input } from '@angular/core';

@Component({
  selector: 'app-page-frame',
  templateUrl: './page-frame.html',
  styleUrl: './page-frame.scss'
})
export class PageFrameComponent {
  readonly eyebrow = input.required<string>();
  readonly title = input.required<string>();
  readonly description = input<string>();
}
