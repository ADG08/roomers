export interface PersonInfo {
  id: string;
  name: string;
}

export interface RoomSnapshot {
  name: string;
  maxCapacity: number;
  isLocked: boolean;
  teacher: PersonInfo | null;
  students: PersonInfo[];
  courseEndsAt: string | null;
}