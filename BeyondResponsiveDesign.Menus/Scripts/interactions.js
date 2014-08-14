(function (interactions) {

    interactions.init = function() {
        $(document.body).on('click', 'nav>ul>li.menuToggle', function(e) {
            $(this).trigger('blur');
            e.preventDefault();
            e.stopPropagation();
            interactions.HidePopups(e);
            interactions.ToggleSideBar(e);
        });

        $(document.body).on('click', 'article>section>nav>ul>li.toolbar1', function (e) {
            $(this).trigger('blur');
            e.preventDefault();
            e.stopPropagation();

            $("#contentOne").removeClass("offLeft");
            $("#contentOne").addClass("active");
            //$("#contentTwo").removeClass("active");
            $("#contentTwo").addClass("offRight");
        });

        $(document.body).on('click', 'article>section>nav>ul>li.toolbar2', function (e) {
            $(this).trigger('blur');
            e.preventDefault();
            e.stopPropagation();

            $("#contentOne").addClass("offLeft");
            //$("#contentOne").removeClass("active");
            $("#contentTwo").removeClass("offRight");
            $("#contentTwo").addClass("active");
        });
    };

    interactions.ToggleSideBar = function (e) {
        if ($('body').hasClass('sidebar')) {
            interactions.HideSideBar(e);
        } else {
            interactions.ShowSideBar(e);
        }
    };

    interactions.ShowSideBar = function (e) {
        $('body').addClass('sidebar');
        $(window).trigger('resize');
    };

    interactions.HideSideBar = function (e) {
        $('body').removeClass('sidebar');
        $(window).trigger('resize');
        return false;
    };

    interactions.HidePopups = function(e) {

    };

})(window.interactions = window.interactions || {});