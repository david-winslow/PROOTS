



import { NgModule } from "@angular/core";
import { FlexLayoutModule } from '@angular/flex-layout'
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';


import { OTSProMaterialModule } from "../modules/material.module";

import { PageHeaderComponent } from './page-header.component'
import { UserEditorComponent } from '../admin/user-editor.component';
import { AppDialogComponent } from './app-dialog.component';

import { GroupByPipe } from '../pipes/group-by.pipe';

@NgModule({
    imports: [
        FlexLayoutModule,
        FormsModule, ReactiveFormsModule,
        BrowserModule, BrowserAnimationsModule,
        OTSProMaterialModule,
    ],
    exports: [
        FlexLayoutModule,
        FormsModule, ReactiveFormsModule,
        BrowserModule, BrowserAnimationsModule,
        OTSProMaterialModule,
        PageHeaderComponent,
        GroupByPipe,
        UserEditorComponent,
        AppDialogComponent
    ],
    declarations: [
        PageHeaderComponent,
        GroupByPipe,
        UserEditorComponent,
        AppDialogComponent
    ],
    entryComponents: [
        AppDialogComponent
    ]
})
export class SharedModule {

}
