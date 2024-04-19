function ActivePScroll() {
    const ps = new PerfectScrollbar('.app-sidebar', {
        useBothWheelAxes: true,
        suppressScrollX: true,
        suppressScrollY: false,
    });
    const ps2 = new PerfectScrollbar('.notifications-menu', {
        useBothWheelAxes: true,
        suppressScrollX: true,
        suppressScrollY: false,
    });
    //const ps3 = new PerfectScrollbar('.message-menu-scroll', {
    //    useBothWheelAxes: true,
    //    suppressScrollX: true,
    //    suppressScrollY: false,
    //});
    const ps11 = new PerfectScrollbar('.sidebar-right', {
        useBothWheelAxes: true,
        suppressScrollX: true,
    });
}
