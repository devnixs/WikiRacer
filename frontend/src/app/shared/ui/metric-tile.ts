import { Component, input } from '@angular/core';

@Component({
  selector: 'app-metric-tile',
  templateUrl: './metric-tile.html',
  styleUrl: './metric-tile.scss'
})
export class MetricTileComponent {
  readonly label = input.required<string>();
  readonly value = input.required<string>();
  readonly hint = input<string>();
}
