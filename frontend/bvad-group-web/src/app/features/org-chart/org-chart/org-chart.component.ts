import { CommonModule } from '@angular/common';
import { Component, effect, inject, signal } from '@angular/core';
import { AuthService } from '../../../core/services/auth.service';
import { OrgChartService } from '../../../core/services/org-chart.service';
import { OrgNode } from '../../../core/models/org-chart.model';
import { OrgNodeComponent } from '../org-node/org-node.component';

@Component({
  selector: 'app-org-chart',
  standalone: true,
  imports: [CommonModule, OrgNodeComponent],
  templateUrl: './org-chart.component.html'
})
export class OrgChartComponent {
  auth = inject(AuthService);
  private service = inject(OrgChartService);

  roots = signal<OrgNode[]>([]);
  loading = signal(false);
  zoom = signal(100);

  constructor() {
    effect(() => {
      const c = this.auth.currentCompany();
      if (c) this.load();
    }, { allowSignalWrites: true });
  }

  load(): void {
    const c = this.auth.currentCompany();
    if (!c) return;

    this.loading.set(true);
    this.service.getOrgChart(c.id).subscribe({
      next: (data) => {
        this.roots.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        console.error(err);
        this.loading.set(false);
      }
    });
  }

  zoomIn(): void {
    this.zoom.update(z => Math.min(z + 10, 150));
  }

  zoomOut(): void {
    this.zoom.update(z => Math.max(z - 10, 50));
  }

  resetZoom(): void {
    this.zoom.set(100);
  }
}