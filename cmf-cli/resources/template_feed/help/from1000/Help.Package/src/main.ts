import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
import { loadApplicationConfig } from 'cmf-core/init';

loadApplicationConfig('assets/config.json').then(() => {
  import(/* webpackMode: "eager" */ './app/app.module').then((m) => {
    platformBrowserDynamic()
      .bootstrapModule(m.AppModule)
      .catch((err) => console.error(err));
  });
});
