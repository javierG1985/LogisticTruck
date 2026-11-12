// @ts-check
const eslint = require('@eslint/js');
const { defineConfig } = require('eslint/config');
const tseslint = require('typescript-eslint');
const angular = require('angular-eslint');

// Reglas de dependencia por carpeta. Los alias (@core, @shared, @features, @env) se revisan como texto del import.
// Dentro de una feature, sus propios archivos se importan con rutas relativas: los alias de features quedan prohibidos.
const restrictedImports = (patterns) => ({
  'no-restricted-imports': ['error', { patterns }],
});

module.exports = defineConfig([
  {
    files: ['**/*.ts'],
    extends: [
      eslint.configs.recommended,
      tseslint.configs.recommended,
      tseslint.configs.stylistic,
      angular.configs.tsRecommended,
    ],
    processor: angular.processInlineTemplates,
    rules: {
      '@angular-eslint/directive-selector': [
        'error',
        {
          type: 'attribute',
          prefix: 'lt',
          style: 'camelCase',
        },
      ],
      '@angular-eslint/component-selector': [
        'error',
        {
          type: 'element',
          prefix: 'lt',
          style: 'kebab-case',
        },
      ],
    },
  },
  {
    // shared no depende de la aplicación: solo de sí mismo.
    files: ['src/app/shared/**/*.ts'],
    rules: restrictedImports([
      {
        group: ['@core/*', '@features/*', '@env/*'],
        message:
          'shared no puede importar core, features ni el entorno. Lo que necesite va como input o como contenido proyectado.',
      },
    ]),
  },
  {
    // core conoce shared, pero no las features.
    files: ['src/app/core/**/*.ts'],
    rules: restrictedImports([
      {
        group: ['@features/*'],
        message:
          'core no puede importar features. Si core necesita algo de una feature, debe llegar por configuración o rutas.',
      },
    ]),
  },
  {
    // Una feature no importa de otra. Lo compartido va a shared/ o core/.
    files: ['src/app/features/**/*.ts'],
    rules: restrictedImports([
      {
        group: ['@features/*'],
        message:
          'Una feature no importa de otra. Dentro de la misma feature usa rutas relativas; lo compartido va a shared/ o core/.',
      },
    ]),
  },
  {
    files: ['**/*.html'],
    extends: [angular.configs.templateRecommended, angular.configs.templateAccessibility],
    rules: {},
  },
]);
