import { CommonModule } from '@angular/common';
import { Component, ElementRef, HostListener, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { NotificationService } from '../../../core/services/notification.service';
import { Notification } from '../../../core/models/notification.model';

@Component({
  selector: 'app-notification-bell',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './notification-bell.component.html'
})
export class NotificationBellComponent {
  notif = inject(NotificationService);
  private router = inject(Router);
  private elementRef = inject(ElementRef);

  open = signal(false);

  toggle(): void {
    this.open.update(v => !v);
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: Event): void {
    if (!this.elementRef.nativeElement.contains(event.target)) {
      this.open.set(false);
    }
  }

  onNotificationClick(n: Notification): void {
    // Marquer comme lu
    if (!n.isRead) {
      this.notif.markAsRead(n.id).subscribe();
    }

    // Rediriger
    if (n.actionUrl) {
      this.router.navigateByUrl(n.actionUrl);
    }

    this.open.set(false);
  }

  markAllRead(event: Event): void {
    event.stopPropagation();
    this.notif.markAllAsRead().subscribe();
  }

  delete(n: Notification, event: Event): void {
    event.stopPropagation();
    this.notif.delete(n.id).subscribe();
  }

  formatTime(dateStr: string): string {
    const date = new Date(dateStr);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMin = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);

    if (diffMin < 1) return 'À l\'instant';
    if (diffMin < 60) return `Il y a ${diffMin} min`;
    if (diffHours < 24) return `Il y a ${diffHours}h`;
    if (diffDays < 7) return `Il y a ${diffDays}j`;
    return date.toLocaleDateString('fr-FR', { day: '2-digit', month: 'short' });
  }
}