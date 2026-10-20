// Desarrollo (ng serve): el front en :4200 llama directo a la API en :5062 (requiere CORS).
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5062',
};
