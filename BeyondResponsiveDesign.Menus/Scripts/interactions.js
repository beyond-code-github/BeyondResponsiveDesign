(function (interactions) {

    interactions.init = function() {
        $(document.body).on('click', 'nav>ul>li.menuToggle', function(e) {
            $(this).trigger('blur');
            e.preventDefault();
            e.stopPropagation();
            interactions.HidePopups(e);
            interactions.ToggleSideBar(e);
        });
    };

    interactions.ToggleSideBar = function (e) {
        if ($('body').hasClass('sidebar') || !$('body').hasClass('sidebarCollapse')) {
            interactions.HideSideBar(e);
        } else {
            interactions.ShowSideBar(e);
        }
    };

    interactions.ShowSideBar = function (e) {
        $('body').addClass('sidebar');
        $('body').removeClass('sidebarCollapse');
        $(window).trigger('resize');
    };

    interactions.HideSideBar = function (e) {
        $('body').removeClass('sidebar');
        $('body').addClass('sidebarCollapse');
        $(window).trigger('resize');
        return false;
    };

    interactions.HidePopups = function(e) {

    };

})(window.interactions = window.interactions || {});