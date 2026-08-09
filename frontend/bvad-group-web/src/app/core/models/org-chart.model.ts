export interface OrgNode {
  id: string;
  fullName: string;
  position: string;
  department?: string;
  email?: string;
  photoUrl?: string;
  phoneNumber?: string;
  companyRole: string;
  isCommitteeMember: boolean;
  committeePosition?: string;
  companyColor: string;
  children: OrgNode[];
  totalSubordinates: number;
}