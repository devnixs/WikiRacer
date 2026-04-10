import { Component, input } from '@angular/core';

@Component({
  selector: 'app-feature-card',
  templateUrl: './feature-card.html',
  styleUrl: './feature-card.scss'
})
export class FeatureCardComponent {
  readonly label = input.required<string>();
  readonly title = input.required<string>();
  readonly body = input.required<string>();
}
