import React, { useEffect } from 'react';
import logo from './logo.svg';
import './App.css';
import { Route, Switch, withRouter, Redirect } from 'react-router-dom'
import Layout from './hoc/Layout/Layout'
import Home from './components/Home';
import Users from './components/Users';

const App = (props) => {

  useEffect(() => {
    document.documentElement.classList.add(props.buildType);
  }, [])

  return (
    <Layout buildType={props.buildType}>
      <Switch>
        <Route path="/" exact component={Home} />
        <Route path="/home" render={(props) => <Home {...props} />} />
        <Route path="/users" component={Users} />
        <Redirect to="/" />
      </Switch>
      <div className="devLabel">{props.buildType}</div>
    </Layout>
  );
}

export default withRouter(App);
