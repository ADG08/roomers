import { computed, DestroyRef, inject, Injectable, signal } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { RoomSnapshot } from './room.model';

type ConnectionState = 'connecting' | 'connected' | 'disconnected';

const HUB_URL = 'http://localhost:5025/hub/rooms';
const RETRY_DELAY_MS = 5000;

@Injectable({ providedIn: 'root' })
export class RoomsService {
  private readonly _rooms = signal<RoomSnapshot[]>([]);
  private readonly _state = signal<ConnectionState>('connecting');
  private readonly _loading = signal(true);

  readonly rooms = this._rooms.asReadonly();
  readonly connectionState = this._state.asReadonly();
  readonly loading = this._loading.asReadonly();

  readonly connected = computed(() => this._state() === 'connected');
  readonly unreachable = computed(() => this._loading() && this._state() === 'disconnected');
  readonly connectionLabel = computed(() => {
    switch (this._state()) {
      case 'connected':
        return 'Connecté';
      case 'connecting':
        return 'Connexion…';
      default:
        return 'Déconnecté';
    }
  });

  private readonly connection: HubConnection = new HubConnectionBuilder()
    .withUrl(HUB_URL)
    .withAutomaticReconnect()
    .build();

  private retryTimer: ReturnType<typeof setTimeout> | undefined;
  private stopped = false;

  constructor() {
    inject(DestroyRef).onDestroy(() => this.stop());

    this.connection.on('RoomsState', (rooms: RoomSnapshot[]) => {
      this._rooms.set(rooms);
      this._loading.set(false);
    });

    this.connection.on('RoomChanged', (room: RoomSnapshot) => {
      this._rooms.update((list) => list.map((r) => (r.name === room.name ? room : r)));
    });

    this.connection.onreconnecting(() => this._state.set('connecting'));
    this.connection.onreconnected(() => this._state.set('connected'));
    this.connection.onclose(() => {
      this._state.set('disconnected');
      this.scheduleRetry();
    });
  }

  async start(): Promise<void> {
    this.stopped = false;
    this._state.set('connecting');
    try {
      await this.connection.start();
      this._state.set('connected');
    } catch (err) {
      this._state.set('disconnected');
      console.error(`Connexion au hub impossible, nouvel essai dans ${RETRY_DELAY_MS / 1000} s`, err);
      this.scheduleRetry();
    }
  }

  stop(): void {
    this.stopped = true;
    clearTimeout(this.retryTimer);
    void this.connection.stop();
  }

  private scheduleRetry(): void {
    if (this.stopped) return;
    clearTimeout(this.retryTimer);
    this.retryTimer = setTimeout(() => void this.start(), RETRY_DELAY_MS);
  }
}