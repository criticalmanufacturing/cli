import { NgModule, isDevMode } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { ServiceWorkerModule } from '@angular/service-worker';

import { AppComponent } from './app.component';

import { CoreModule, MetadataRoutingModule } from 'cmf-core';
import { CoreControlsMetadataModule } from 'cmf-core-controls/metadata';

import { DocsControlsMetadataModule } from 'cmf-docs-controls/metadata';

import { DocsAreaDevelopmentMetadataModule } from 'cmf-docs-area-development/metadata';
import { DocsShellMetadataModule } from 'cmf-docs-shell/metadata';
import { DocsAreaDesignsystemMetadataModule } from 'cmf-docs-area-designsystem/metadata';

@NgModule({
    declarations: [
        AppComponent
    ],
    imports: [
        BrowserModule,
        CoreControlsMetadataModule,
        CoreModule,
        DocsShellMetadataModule,
        DocsControlsMetadataModule,
        DocsAreaDevelopmentMetadataModule,
        DocsAreaDesignsystemMetadataModule,
        // start: add your custom doc package modules here
        // end: add your custom doc package modules here
        MetadataRoutingModule,
        ServiceWorkerModule.register('ngsw-worker.js', {
            enabled: !isDevMode(),
            // Register the ServiceWorker as soon as the application is stable
            // or after 30 seconds (whichever comes first).
            registrationStrategy: 'registerWhenStable:30000'
        })
    ],
    providers: [],
    bootstrap: [AppComponent]
})
export class AppModule { }
