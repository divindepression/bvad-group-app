import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Notification, NotificationCount } from '../models/notification.model';
import { SignalRService } from './signalr.service';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private apiUrl = `${environment.apiUrl}/notifications`;
  private http = inject(HttpClient);
  private signalR = inject(SignalRService);

  // 🔔 État réactif
  notifications = signal<Notification[]>([]);
  unreadCount = computed(() => this.notifications().filter(n => !n.isRead).length);
  hasUnread = computed(() => this.unreadCount() > 0);

  // Toast temporaire pour les nouvelles notifs
  currentToast = signal<Notification | null>(null);
  private toastTimeout: any = null;

  constructor() {
    // 🔔 Écoute des événements SignalR
    this.signalR.notificationReceived$.subscribe(notif => {
      this.notifications.update(list => [notif, ...list]);
      this.showToast(notif);
    });

    this.signalR.notificationRead$.subscribe(id => {
      this.notifications.update(list =>
        list.map(n => n.id === id ? { ...n, isRead: true } : n)
      );
    });

    this.signalR.allRead$.subscribe(() => {
      this.notifications.update(list =>
        list.map(n => ({ ...n, isRead: true }))
      );
    });

    this.signalR.notificationDeleted$.subscribe(id => {
      this.notifications.update(list => list.filter(n => n.id !== id));
    });
  }

  // ═══════════════════════════════════════
  // API
  // ═══════════════════════════════════════
  loadAll(take: number = 50): Observable<Notification[]> {
    return this.http.get<Notification[]>(`${this.apiUrl}?take=${take}`).pipe(
      tap(list => this.notifications.set(list))
    );
  }

  getCount(): Observable<NotificationCount> {
    return this.http.get<NotificationCount>(`${this.apiUrl}/count`);
  }

  markAsRead(id: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/read`, {}).pipe(
      tap(() => {
        this.notifications.update(list =>
          list.map(n => n.id === id ? { ...n, isRead: true } : n)
        );
      })
    );
  }

  markAllAsRead(): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/read-all`, {}).pipe(
      tap(() => {
        this.notifications.update(list =>
          list.map(n => ({ ...n, isRead: true }))
        );
      })
    );
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`).pipe(
      tap(() => {
        this.notifications.update(list => list.filter(n => n.id !== id));
      })
    );
  }

  // ═══════════════════════════════════════
  // Toast temporaire
  // ═══════════════════════════════════════
  private showToast(notif: Notification): void {
    if (this.toastTimeout) clearTimeout(this.toastTimeout);
    this.currentToast.set(notif);
    this.toastTimeout = setTimeout(() => this.currentToast.set(null), 5000);
  }

  closeToast(): void {
    if (this.toastTimeout) clearTimeout(this.toastTimeout);
    this.currentToast.set(null);
  }

  // ═══════════════════════════════════════
  // Reset (logout)
  // ═══════════════════════════════════════
  reset(): void {
    this.notifications.set([]);
    this.currentToast.set(null);
  }
}