import { Component, input } from '@angular/core';

@Component({
  selector: 'app-chip',
  templateUrl: './chip.html',
  styleUrl: './chip.scss'
})
export class ChipComponent {
  readonly text = input.required<string>();
  readonly contrast = input(false);
}
