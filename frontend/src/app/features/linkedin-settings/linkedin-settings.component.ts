import { Component } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { DividerModule } from 'primeng/divider';
import { MessageModule } from 'primeng/message';
import { TagModule } from 'primeng/tag';

@Component({
  selector: 'app-linkedin-settings',
  imports: [ButtonModule, CardModule, DividerModule, MessageModule, TagModule],
  templateUrl: './linkedin-settings.component.html',
  styleUrl: './linkedin-settings.component.scss'
})
export class LinkedinSettingsComponent {}
