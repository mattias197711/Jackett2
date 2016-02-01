import {Router, RouterConfiguration} from 'aurelia-router'

export class App {
  router: Router;
  
  configureRouter(config: RouterConfiguration, router: Router) {
    config.title = 'Jackett 2';
    config.map([
        { route: ['', 'welcome'], name: 'welcome', moduleId: 'home', nav: true, title: 'Home' },
        { route: ['indexers'], name: 'indexers', moduleId: 'indexers', nav: true,  title: 'Indexers' },
        { route: ['irc'], name: 'irc', moduleId: 'irc', nav: true, title: 'IRC' },
        { route: ['settings'], name: 'settings', moduleId: 'settings', nav: true,  title: 'Settings' },
        { route: ['settings'], name: 'settings', moduleId: 'settings', title: 'Server settings' },
        { route: 'irc-settings', name: 'irc-settings', moduleId: 'irc-settings', title: 'IRC Settings' },
        { route: 'irc-profile-edit/:name', name: 'irc-profile-edit', moduleId: 'irc-settings-edit', title: 'Edit Profile' },
        { route: 'irc-profile-create', name: 'irc-profile-create', moduleId: 'irc-settings-edit', title: 'Create Profile' },
        { route: 'autodlprofile-configure/:type', name: 'autodlprofile-configure', moduleId: 'autodlprofile-configure', title: 'AutoDL Profile Config' }
    ]);

    this.router = router;
  }
}
  