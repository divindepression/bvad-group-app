import { Injectable, inject } from '@angular/core';
import { HubConnection, HubConnectionBuilder, HubConnectionState, LogLevel } from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Notification } from '../models/notification.model';
import { TokenService } from './token.service';

@Injectable({ providedIn: 'root' })
export class SignalRService {
  private hubConnection: HubConnection | null = null;
  private tokenService = inject(TokenService);

  // ═══ Événements exposés en Subjects ═══
  notificationReceived$ = new Subject<Notification>();
  notificationRead$ = new Subject<string>();
  allRead$ = new Subject<void>();
  notificationDeleted$ = new Subject<string>();

  /**
   * Démarre la connexion SignalR.
   * À appeler après le login.
   */
  async start(): Promise<void> {
    if (this.hubConnection?.state === HubConnectionState.Connected) {
      return;
    }

    const token = this.tokenService.getToken();
    if (!token) {
      console.warn('⚠ SignalR : pas de token, connexion annulée');
      return;
    }

    // Construction de l'URL du hub
    const hubUrl = environment.apiUrl.replace('/api', '') + '/hubs/notifications';

    this.hubConnection = new HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(LogLevel.Warning)
      .build();

    // ═══ Événements du hub ═══
    this.hubConnection.on('ReceiveNotification', (notif: Notification) => {
      console.log('🔔 Nouvelle notification :', notif);
      this.notificationReceived$.next(notif);
    });

    this.hubConnection.on('NotificationRead', (id: string) => {
      this.notificationRead$.next(id);
    });

    this.hubConnection.on('AllNotificationsRead', () => {
      this.allRead$.next();
    });

    this.hubConnection.on('NotificationDeleted', (id: string) => {
      this.notificationDeleted$.next(id);
    });

    this.hubConnection.onreconnecting(() => {
      console.warn('⚠ SignalR : reconnexion...');
    });

    this.hubConnection.onreconnected(() => {
      console.log('✅ SignalR : reconnecté');
    });

    this.hubConnection.onclose(() => {
      console.log('🔌 SignalR : fermé');
    });

    try {
      await this.hubConnection.start();
      console.log('✅ SignalR connecté sur', hubUrl);
    } catch (err) {
      console.error('❌ SignalR : erreur de connexion', err);
    }
  }

  /**
   * Arrête la connexion (appelé au logout).
   */
  async stop(): Promise<void> {
    if (this.hubConnection) {
      await this.hubConnection.stop();
      this.hubConnection = null;
    }
  }

  isConnected(): boolean {
    return this.hubConnection?.state === HubConnectionState.Connected;
  }
}