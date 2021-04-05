import React from 'react';
import CssBaseline from '@material-ui/core/CssBaseline';
import Typography from '@material-ui/core/Typography';
import Container from '@material-ui/core/Container';
import { Link, } from 'react-router-dom'

const Layout = (props) => {

    console.log(props)
    return (
        <main>
            <nav>
                <ul>
                    <li>
                        <Link
                            to="/"
                        >
                            Home
                        </Link>
                    </li>
                    <li>
                        <Link
                            to="/users"
                        >
                            Users
                        </Link>
                    </li>
                </ul>


            </nav>
            <section>
                {props.children}
            </section>
        </main>
    )
}

export default Layout
