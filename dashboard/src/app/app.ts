import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { RoomCard } from './room-card';
import { RoomsService } from './rooms.service';

@Component({
  selector: 'app-root',
  imports: [RoomCard],
  templateUrl: './app.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class App implements OnInit {
  protected readonly service = inject(RoomsService);

  ngOnInit(): void {
    void this.service.start();
  }
}