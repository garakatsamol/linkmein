import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';

import { AppearanceService } from './core/appearance/appearance.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  private readonly appearance = inject(AppearanceService);
}
