import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';

interface NavItem {
  label: string;
  icon: string;
  route: string;
  disabled?: boolean;
}

@Component({
  selector: 'app-layout',
  imports: [ButtonModule, RouterLink, RouterLinkActive, RouterOutlet, TagModule],
  templateUrl: './app-layout.component.html',
  styleUrl: './app-layout.component.scss'
})
export class AppLayoutComponent {
  protected readonly navItems: NavItem[] = [
    { label: 'Dashboard', icon: 'pi pi-home', route: '/dashboard' },
    { label: 'Calendar', icon: 'pi pi-calendar', route: '/calendar', disabled: true },
    { label: 'Posts', icon: 'pi pi-list', route: '/posts', disabled: true },
    { label: 'Composer', icon: 'pi pi-pencil', route: '/composer', disabled: true },
    { label: 'LinkedIn', icon: 'pi pi-link', route: '/linkedin', disabled: true }
  ];
}
