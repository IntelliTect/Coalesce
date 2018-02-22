<template>


    <ul class="nav nav-pills flex-column">
        <li class="nav-item">
            <router-link class="nav-link" to="/" :exact="true">
                <span class="glyphicon glyphicon-home"></span> Home
            </router-link>
        </li>
        <li class="nav-item">
            <router-link class="nav-link" to="/event">
                <span class="glyphicon glyphicon-education"></span> Event
            </router-link>
        </li>
        <li class="nav-item">
            <router-link class="nav-link" to="/fetchdata">
                <span class="glyphicon glyphicon-th-list"></span> Fetch data
            </router-link>
        </li>
    </ul>
</template>

<style>
    .main-nav li .glyphicon {
        margin-right: 10px;
    }

    /* Highlighting rules for nav menu items */
    .main-nav li a.router-link-active,
    .main-nav li a.router-link-active:hover,
    .main-nav li a.router-link-active:focus {
        background-color: #4189C7;
        color: white;
    }

    /* Keep the nav menu independent of scrolling and on top of other items */
    .main-nav {
        position: fixed;
        top: 0;
        left: 0;
        right: 0;
        z-index: 1;
    }

    @media (min-width: 768px) {
        /* On small screens, convert the nav menu to a vertical sidebar */
        .main-nav {
            height: 100%;
            width: 200px;
        }

        .main-nav .navbar {
            border-radius: 0px;
            border-width: 0px;
            height: 100%;
        }

        .main-nav .navbar-header {
            float: none;
        }

        .main-nav .navbar-collapse {
            border-top: 1px solid #444;
            padding: 0px;
        }

        .main-nav .navbar ul {
            float: none;
        }

        .main-nav .navbar li {
            float: none;
            font-size: 15px;
            margin: 6px;
        }

        .main-nav .navbar li a {
            padding: 10px 16px;
            border-radius: 4px;
        }

        .main-nav .navbar a {
            /* If a menu item's text is too long, truncate it */
            width: 100%;
            white-space: nowrap;
            overflow: hidden;
            text-overflow: ellipsis;
        }
    }

</style>
