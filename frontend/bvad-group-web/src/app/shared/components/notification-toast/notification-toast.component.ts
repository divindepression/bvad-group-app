import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-notification-toast',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './notification-toast.component.html',
  styles: [`
    @keyframes slideIn {
      from { transform: translateX(120%); opacity: 0; }
      to { transform: translateX(0); opacity: 1; }
    }
    .toast-anim {
      animation: slideIn 0.3s ease-out;
    }
  `]
})
export class NotificationToastComponent {
  notif = inject(NotificationService);
  private router = inject(Router);

  onClick(): void {
    const t = this.notif.currentToast();
    if (!t) return;

    if (!t.isRead) this.notif.markAsRead(t.id).subscribe();
    if (t.actionUrl) this.router.navigateByUrl(t.actionUrl);
    this.notif.closeToast();
  }

  close(event: Event): void {
    event.stopPropagation();
    this.notif.closeToast();
  }
}