import { InjectionToken } from '@angular/core';

// Base de la API. Se toma del entorno en app.config.ts; los servicios la inyectan con este token.
export const API_BASE_URL = new InjectionToken<string>('API_BASE_URL');
