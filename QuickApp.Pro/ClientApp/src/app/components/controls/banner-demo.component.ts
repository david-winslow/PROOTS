// =============================
// Email: info@ebenmonney.com
// www.ebenmonney.com/templates
// =============================

import { Component, OnInit } from '@angular/core';

import { NgxCarousel } from 'ngx-carousel';

@Component({
    selector: 'banner-demo',
    templateUrl: './banner-demo.component.html',
    styleUrls: ['./banner-demo.component.scss']
})
export class BannerDemoComponent implements OnInit
{
    banner1 = require("../../assets/images/demo/banner1.png");
    banner2 = require("../../assets/images/demo/banner2.png");
    banner3 = require("../../assets/images/demo/banner3.png");
    banner4 = require("../../assets/images/demo/banner4.png");

    public carousel: NgxCarousel;

    ngOnInit()
    {
        this.carousel = {
            grid: { xs: 1, sm: 1, md: 1, lg: 1, all: 0 },
            slide: 4,
            speed: 500,
            interval: 5000,
            point: { visible: true, pointStyles: '.ngxcarouselPoint{}' },
            load: 2,
            custom: 'banner',
            touch: true,
            loop: true,
            easing: 'cubic-bezier(0, 0, 0.2, 1)'
        };
    }
}
