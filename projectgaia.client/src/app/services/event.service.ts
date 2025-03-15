import { Injectable } from '@angular/core';

export interface Event {
  id: number;
  name: string;
  description: string;
  type: EventType;
}
export enum EventType {
  Payment = 'Payment',
  Price = 'Price',
  Miscellaneous = 'Miscellaneous',
}

@Injectable({
  providedIn: 'root'
})
export class EventService {

}
