import { Injectable, signal } from '@angular/core';
import { HubConnectionBuilder } from '@microsoft/signalr';
import { RoomSnapshot } from './room.model';

@Injectable({ providedIn: 'root' })
export class RoomsService {
  readonly rooms = signal<RoomSnapshot[]>([]);

  private readonly connection = new HubConnectionBuilder()
    .withUrl('http://localhost:5025/hub/rooms')
    .withAutomaticReconnect()
    .build();

  constructor() {
    this.connection.on('RoomsState', (rooms: RoomSnapshot[]) => {
      this.rooms.set(rooms);
    });

    this.connection.on('RoomChanged', (room: RoomSnapshot) => {
      this.rooms.update((list) => list.map((r) => (r.name === room.name ? room : r)));
    });
  }

  start(): void {
    this.connection.start().catch((err) => console.error('Connexion au hub impossible', err));
  }
}