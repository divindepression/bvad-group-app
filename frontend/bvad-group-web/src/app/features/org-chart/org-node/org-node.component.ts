import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { OrgNode } from '../../../core/models/org-chart.model';

@Component({
  selector: 'app-org-node',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './org-node.component.html'
})
export class OrgNodeComponent {
  @Input({ required: true }) node!: OrgNode;
  @Input() isRoot = false;

  initials(name: string): string {
    const parts = name.trim().split(' ');
    if (parts.length >= 2) {
      return (parts[0].charAt(0) + parts[parts.length - 1].charAt(0)).toUpperCase();
    }
    return name.charAt(0).toUpperCase();
  }

  positionIcon(pos?: string): string {
    const map: Record<string, string> = {
      CEO: '👑', DGA: '⭐', CFO: '💰', CHRO: '💼',
      CTO: '🎯', COO: '⚙️', CMO: '📈', CIO: '💻', Legal: '⚖️'
    };
    return pos ? (map[pos] || '') : '';
  }
}