import { Component, OnInit, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AuthService } from './core/services/auth.service';
import { SignalRService } from './core/services/signalr.service';
import { NotificationService } from './core/services/notification.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  template: '<router-outlet></router-outlet>'
})
export class AppComponent implements OnInit {
  private auth = inject(AuthService);
  private signalR = inject(SignalRService);
  private notifService = inject(NotificationService);

  ngOnInit(): void {
    // ═══ 1. Si déjà connecté (refresh de page) ═══
    if (this.auth.isLoggedIn()) {
      this.signalR.start();
      this.notifService.loadAll().subscribe();
    }

    // ═══ 2. Écouter les événements login/logout ═══
    this.auth.loginSuccess$.subscribe(() => {
      this.signalR.start();
      this.notifService.loadAll().subscribe();
    });

    this.auth.logoutSuccess$.subscribe(() => {
      this.signalR.stop();
      this.notifService.reset();
    });
  }
}