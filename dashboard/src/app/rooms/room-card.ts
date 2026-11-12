import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { Clock } from './clock';
import { RoomSnapshot } from './room.model';

@Component({
  selector: 'app-room-card',
  templateUrl: './room-card.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'flex min-w-64 flex-1' },
})
export class RoomCard {
  private readonly clock = inject(Clock);

  readonly room = input.required<RoomSnapshot>();

  protected readonly occupied = computed(() => {
    const room = this.room();
    return room.students.length + (room.teacher ? 1 : 0);
  });

  protected readonly remaining = computed(() => {
    const endsAt = this.room().courseEndsAt;
    if (!endsAt) return null;

    const seconds = Math.max(0, Math.ceil((new Date(endsAt).getTime() - this.clock.now()) / 1000));
    const minutes = Math.floor(seconds / 60);
    const rest = seconds % 60;
    return `${String(minutes).padStart(2, '0')}:${String(rest).padStart(2, '0')}`;
  });
}