export type LeaveStatus = 'Pending' | 'Approved' | 'Rejected' | 'Cancelled' | 'Taken';

export interface LeaveType {
  id: string;
  code: string;
  name: string;
  description?: string;
  icon: string;
  color: string;
  defaultDaysPerYear: number;
  daysAccruedPerMonth: number;
  isPaid: boolean;
  requiresProof: boolean;
  decrementsBalance: boolean;
  displayOrder: number;
  isActive: boolean;
}

export interface LeaveBalance {
  id: string;
  employeeId: string;
  employeeName: string;
  leaveTypeId: string;
  leaveTypeName: string;
  leaveTypeIcon: string;
  leaveTypeColor: string;
  year: number;
  allocatedDays: number;
  usedDays: number;
  carriedOverDays: number;
  adjustment: number;
  remainingDays: number;
}

export interface LeaveRequest {
  id: string;
  employeeId: string;
  employeeName: string;
  employeePhotoUrl?: string;
  companyId: string;
  companyName: string;
  companyColor: string;
  leaveTypeId: string;
  leaveTypeCode: string;
  leaveTypeName: string;
  leaveTypeIcon: string;
  leaveTypeColor: string;
  startDate: string;
  endDate: string;
  daysCount: number;
  isHalfDay: boolean;
  reason?: string;
  proofDocumentUrl?: string;
  proofDocumentName?: string;
  status: LeaveStatus;
  approvedByUserId?: string;
  approvedByName?: string;
  approvedAt?: string;
  approvalComment?: string;
  isPast: boolean;
  isCurrent: boolean;
  isFuture: boolean;
  createdAt: string;
}

export interface CalendarLeave {
  id: string;
  employeeId: string;
  employeeName: string;
  leaveTypeCode: string;
  leaveTypeName: string;
  leaveTypeIcon: string;
  leaveTypeColor: string;
  startDate: string;
  endDate: string;
  daysCount: number;
  status: string;
}

export interface CreateLeaveRequest {
  employeeId: string;
  leaveTypeId: string;
  startDate: string;
  endDate: string;
  isHalfDay: boolean;
  reason?: string;
}

// Helpers
export const LeaveStatusLabels: Record<LeaveStatus, string> = {
  Pending: 'En attente',
  Approved: 'Approuvé',
  Rejected: 'Refusé',
  Cancelled: 'Annulé',
  Taken: 'Effectué'
};

export const LeaveStatusColors: Record<LeaveStatus, string> = {
  Pending: 'bg-orange-500/20 text-orange-400 border-orange-500/40',
  Approved: 'bg-green-500/20 text-green-400 border-green-500/40',
  Rejected: 'bg-red-500/20 text-red-400 border-red-500/40',
  Cancelled: 'bg-slate-500/20 text-slate-400 border-slate-500/40',
  Taken: 'bg-blue-500/20 text-blue-400 border-blue-500/40'
};