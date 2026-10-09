import { Component } from '@angular/core';
import { Order } from './components/order/order';

@Component({
    selector: 'app-root',
    standalone: true,
    imports: [Order],
    templateUrl: './app.html',
    styleUrl: './app.css'
})
export class App {
}
