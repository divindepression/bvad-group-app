export type NotificationType =
  | 'LeaveRequestSubmitted' | 'LeaveRequestApproved' | 'LeaveRequestRejected' | 'LeaveRequestCancelled'
  | 'ContractExpiringSoon' | 'ContractExpired' | 'ContractCreated' | 'ContractRenewed'
  | 'EmployeeHired' | 'EmployeeLeft' | 'EmployeeBirthday'
  | 'SystemAlert' | 'Info';

export type NotificationPriority = 'Low' | 'Normal' | 'High' | 'Urgent';

export interface Notification {
  id: string;
  type: NotificationType;
  priority: NotificationPriority;
  title: string;
  message: string;
  icon: string;
  color: string;
  actionUrl?: string;
  relatedEntityId?: string;
  relatedEntityType?: string;
  isRead: boolean;
  readAt?: string;
  createdAt: string;
}

export interface NotificationCount {
  total: number;
  unread: number;
}