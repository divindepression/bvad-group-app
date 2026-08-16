import { CommonModule } from '@angular/common';
import { Component, HostListener, signal } from '@angular/core';

@Component({
  selector: 'app-install-prompt',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div
      *ngIf="showPrompt()"
      class="fixed bottom-4 left-4 right-4 sm:left-auto sm:right-4 sm:w-96 z-50
             bg-slate-800 border border-bvad-secondary/50 rounded-2xl shadow-2xl p-5
             animate-slide-up"
    >
      <div class="flex items-start gap-4">
        <div class="w-14 h-14 bg-bvad-primary rounded-xl flex items-center justify-center flex-shrink-0">
          <span class="text-3xl">🏢</span>
        </div>
        <div class="flex-1">
          <h3 class="text-white font-bold text-lg">Installer BVAD GROUP</h3>
          <p class="text-slate-400 text-sm mt-1">
            Ajoutez l'application à votre écran d'accueil pour un accès rapide
          </p>
          <div class="flex gap-2 mt-3">
            <button
              (click)="install()"
              class="bg-bvad-secondary hover:bg-amber-600 text-white font-semibold px-4 py-2 rounded-lg text-sm transition"
            >
              📱 Installer
            </button>
            <button
              (click)="dismiss()"
              class="text-slate-400 hover:text-white px-3 py-2 rounded-lg text-sm transition"
            >
              Plus tard
            </button>
          </div>
        </div>
        <button
          (click)="dismiss()"
          class="text-slate-500 hover:text-white text-xl leading-none"
        >×</button>
      </div>
    </div>
  `,
  styles: [`
    @keyframes slide-up {
      from {
        transform: translateY(100px);
        opacity: 0;
      }
      to {
        transform: translateY(0);
        opacity: 1;
      }
    }
    .animate-slide-up {
      animation: slide-up 0.4s ease-out;
    }
  `]
})
export class InstallPromptComponent {
  showPrompt = signal(false);
  private deferredPrompt: any = null;

  @HostListener('window:beforeinstallprompt', ['$event'])
  onBeforeInstallPrompt(event: Event): void {
    event.preventDefault();
    this.deferredPrompt = event;

    // Vérifier si l'utilisateur n'a pas déjà dismissé
    const dismissed = localStorage.getItem('pwa-install-dismissed');
    if (!dismissed) {
      // Attendre 30 secondes avant de montrer
      setTimeout(() => this.showPrompt.set(true), 30000);
    }
  }

  async install(): Promise<void> {
    if (!this.deferredPrompt) return;

    this.deferredPrompt.prompt();
    const result = await this.deferredPrompt.userChoice;

    if (result.outcome === 'accepted') {
      console.log('✅ PWA installée');
    }

    this.deferredPrompt = null;
    this.showPrompt.set(false);
  }

  dismiss(): void {
    this.showPrompt.set(false);
    localStorage.setItem('pwa-install-dismissed', 'true');
  }
}