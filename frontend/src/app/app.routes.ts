import { Routes } from '@angular/router';

import { AppLayoutComponent } from './layout/app-layout.component';

export const routes: Routes = [
  {
    path: '',
    component: AppLayoutComponent,
    children: [
      {
        path: '',
        pathMatch: 'full',
        redirectTo: 'dashboard'
      },
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./features/dashboard/dashboard.component').then((m) => m.DashboardComponent)
      },
      {
        path: 'posts',
        loadComponent: () =>
          import('./features/posts/post-list.component').then((m) => m.PostListComponent)
      },
      {
        path: 'composer',
        loadComponent: () =>
          import('./features/composer/post-composer.component').then((m) => m.PostComposerComponent)
      },
      {
        path: 'composer/:id',
        loadComponent: () =>
          import('./features/composer/post-composer.component').then((m) => m.PostComposerComponent)
      }
    ]
  },
  {
    path: '**',
    redirectTo: 'dashboard'
  }
];
