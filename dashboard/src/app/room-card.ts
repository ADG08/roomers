import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { RoomSnapshot } from './room.model';

@Component({
  selector: 'app-room-card',
  templateUrl: './room-card.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'flex min-w-64 flex-1' },
})
export class RoomCard {
  readonly room = input.required<RoomSnapshot>();

  protected readonly occupied = computed(() => {
    const room = this.room();
    return room.students.length + (room.teacher ? 1 : 0);
  });
}