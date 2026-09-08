import { Landing } from './pages/landing/landing';
import { Login } from './pages/login/login';
export const routes = [
    {
        path: '',
        component: Landing,
        title: 'Eventora | Events • Tickets • Parking',
    },
    {
        path: 'login',
        component: Login,
        title: 'Login | Event Parking Reservation System',
    },
    {
        path: '**',
        redirectTo: '',
    },
];
