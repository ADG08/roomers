import { Component, inject, OnInit } from '@angular/core';
import { JsonPipe } from '@angular/common';
import { RoomsService } from './rooms.service';

@Component({
  selector: 'app-root',
  imports: [JsonPipe],
  templateUrl: './app.html',
})
export class App implements OnInit {
  protected readonly roomsService = inject(RoomsService);

  ngOnInit(): void {
    this.roomsService.start();
  }
}