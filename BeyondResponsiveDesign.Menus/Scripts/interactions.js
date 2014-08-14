(function (interactions) {

    interactions.init = function () {
        $.detectSwipe.preventDefault = false;
        $('body').on('touchmove', function (e) {
            var searchTerms = '.vscroll, .vscroll-scroll, .hscroll',
                $target = $(e.target),
                parents = $target.parents(searchTerms);

            if (parents.length || $target.hasClass(searchTerms)) {
                // ignore as we want the scroll to happen
                // (This is where we may need to check if at limit)
            } else {
                e.preventDefault();
            }
        });

        $(document.body).on('click', 'nav>ul>li.menuToggle', function (e) {
            $(this).trigger('blur');
            e.preventDefault();
            e.stopPropagation();
            interactions.HidePopups(e);
            interactions.ToggleSideBar(e);
        });

        interactions.activateTabOne = function () {
            $("#contentOne").removeClass("offLeft");
            $("#contentOne").addClass("active");
            $("#contentTwo").addClass("offRight");
            $("#contentTwo").removeClass("active");
        }

        interactions.activateTabTwo = function () {
            $("#contentOne").addClass("offLeft");
            $("#contentOne").removeClass("active");
            $("#contentTwo").removeClass("offRight");
            $("#contentTwo").addClass("active");
        }

        $(document.body).on('click', 'article>section>nav>ul>li.toolbar1', function (e) {
            $(this).trigger('blur');
            e.preventDefault();
            e.stopPropagation();

            interactions.activateTabOne();
        });

        $(document.body).on('click', 'article>section>nav>ul>li.toolbar2', function (e) {
            $(this).trigger('blur');
            e.preventDefault();
            e.stopPropagation();

            interactions.activateTabTwo();
        });

        $("article").on('swiperight', function (e) {
            if ($("#contentOne").hasClass("active")) {
                interactions.ShowSideBar(e);
            } else {
                interactions.activateTabOne();
            }
        });

        $("article").on('swipeleft', function (e) {
            if ($('body').hasClass("sidebar")) {
                interactions.HideSideBar(e);
            } else {
                interactions.activateTabTwo();
            }
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

    interactions.HidePopups = function (e) {

    };

})(window.interactions = window.interactions || {});