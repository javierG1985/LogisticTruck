import packageJson from '@package';

// La versión sale del package.json para que no haya dos fuentes de verdad.
export const APP_VERSION: string = packageJson.version;
