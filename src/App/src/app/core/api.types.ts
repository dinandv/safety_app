/**
 * Mirrors src/Api/Contracts/ParticipantContracts.cs.
 *
 * The API deliberately sends facts rather than sentences — a reason
 * kind, a count, a vest colour — so every word the volunteer reads is
 * written here, in one place, in one language.
 */

export type Iso8601 = string;
export type IsoDate = string;

export type OpenSpotReason = 'NeverFilled' | 'Withdrawn';

/** Why phone numbers are, or are not, on the screen. */
export type PhoneVisibilityState = 'Visible' | 'NotScheduled' | 'OutsideShiftWindow';

export interface TeamMember {
  personId: string;
  name: string;
  initials: string;
  /** Null unless the caller is scheduled themselves — see PhoneVisibility. */
  phone: string | null;
  isSelf: boolean;
}

export interface OpenSpots {
  count: number;
  reason: OpenSpotReason;
  withdrawnByFirstName: string | null;
  openCallId: string | null;
}

export interface RoleGroup {
  shiftId: string;
  teamRoleName: string;
  vestColor: string | null;
  start: Iso8601;
  end: Iso8601;
  requiredCount: number;
  people: TeamMember[];
  openSpots: OpenSpots | null;
}

export interface AdvisoryNote {
  id: string;
  title: string;
  text: string;
}

export interface OwnShift {
  shiftId: string;
  assignmentId: string;
  teamRoleName: string;
  vestColor: string | null;
  start: Iso8601;
  end: Iso8601;
  personName: string;
  note: string | null;
}

export interface TodayEvent {
  id: string;
  title: string;
  start: Iso8601;
  end: Iso8601;
  locationName: string;
  filledCount: number;
  requiredCount: number;
  phoneNumbers: PhoneVisibilityState;
  advisories: AdvisoryNote[];
  ownShift: OwnShift | null;
  roleGroups: RoleGroup[];
}

export interface UpcomingEvent {
  id: string;
  title: string;
  start: Iso8601;
  end: Iso8601;
}

export interface TodayResponse {
  date: IsoDate;
  generatedAt: Iso8601;
  event: TodayEvent | null;
  nextEvent: UpcomingEvent | null;
}

export interface MyShift {
  shiftId: string;
  assignmentId: string;
  eventId: string;
  eventTitle: string;
  teamRoleName: string;
  vestColor: string | null;
  start: Iso8601;
  end: Iso8601;
  locationName: string;
  requiredCount: number;
  filledCount: number;
}

export interface OpenCall {
  id: string;
  shiftId: string;
  eventId: string;
  eventTitle: string;
  teamRoleName: string;
  vestColor: string | null;
  start: Iso8601;
  end: Iso8601;
  locationName: string;
  reason: OpenSpotReason;
  withdrawnByFirstName: string | null;
  alreadyOnThisShift: boolean;
}

export interface ContactCardEntry {
  id: string;
  name: string;
  function: string | null;
  phone: string;
  isEmergencyNumber: boolean;
}

export interface GuidelineCard {
  id: string;
  title: string;
  sanitizedHtml: string;
  version: number;
}

export interface CurrentUser {
  personId: string;
  firstName: string;
  lastName: string;
  displayName: string;
  roles: string[];
}
