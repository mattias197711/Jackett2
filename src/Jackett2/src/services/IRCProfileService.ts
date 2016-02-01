import {WebClient} from 'services/WebClient';
import {autoinject} from 'aurelia-framework';
import {HttpClient, HttpResponseMessage} from 'aurelia-http-client';

@autoinject
export class IRCProfileService {
    webClient: WebClient;

    constructor(httpClient: WebClient) {
        this.webClient = httpClient; 
    }

    getAutoDLProfiles(): Promise<NetworkSummary[]> {
        return this.webClient.get('AutoDL/Summary')
            .then(response => { return JSON.parse(response.response); });
    }

    getProfiles(): Promise<IRCProfile[]> {
        return this.webClient.get('IRCProfile')
            .then(response => { return JSON.parse(response.response); });
    }

    getProfile(name: string): Promise<IRCProfile> {
        return this.webClient.get('IRCProfile/' + name)
            .then(response => { return JSON.parse(response.response); });
    }

    setProfile(profile: IRCProfile): Promise<HttpResponseMessage> {
        return this.webClient.put('IRCProfile', profile); 
    }

    removeProfile(profile: IRCProfile): Promise<HttpResponseMessage> {
        return this.webClient.delete('IRCProfile/' + profile.Id);
    }
}


export class NetworkSummary {
    Name: string;
    Servers: string[];
    Profiles: string[];
}

export class IRCProfile {
    Id: string;
    Name: string;
    Servers: string[];
    Username: string;
    Profile: string;
}