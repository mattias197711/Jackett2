﻿import {HttpClient} from 'aurelia-http-client'
import {autoinject} from 'aurelia-framework';

@autoinject 
export class WebClient {
    http: HttpClient;

    constructor(httpClient: HttpClient) { 
        this.http = httpClient;
        this.http.configure(config=> {
            config.withHeader('Content-Type', 'application/json');
            config.withHeader('Accept', 'application/json');
        });
    }

    get(url: string) {
        return this.http.get('../' + url + '?_t=' + new Date().getTime());
    }

    put(url: string, obj: any) {
        return this.http.put('../' + url + '?_t=' + new Date().getTime(), JSON.stringify(obj));
    }

    delete(url: string) {
        return this.http.delete('../' + url + '?_t=' + new Date().getTime());
    }

    post(url: string, obj: any) { 
        return this.http.post('../' + url + '?_t=' + new Date().getTime(), JSON.stringify(obj));
    }
}