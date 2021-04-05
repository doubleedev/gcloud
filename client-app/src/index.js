import React from 'react';
import ReactDOM from 'react-dom';
import reportWebVitals from './reportWebVitals';
import './index.css';
import App from './App';

import { BrowserRouter } from 'react-router-dom';
import { Provider } from 'react-redux'

import { createStore, applyMiddleware, compose, combineReducers } from 'redux'
import thunk from 'redux-thunk'

import usersReducer from './store/reducers/users'

/* eslint-disable-next-line no-undef*/
const buildType = process.env.REACT_APP_BUILD_TYPE;

console.log(process.env);

const composeEnhancers = (process.env.NODE_ENV === 'development' ? window.__REDUX_DEVTOOLS_EXTENSION_COMPOSE__ : null) || compose

const rootReducers = combineReducers({
  users: usersReducer
})

const store = createStore(
  rootReducers,
  composeEnhancers(applyMiddleware(thunk))
)

const app = (
  <Provider store={store}>
    <BrowserRouter>
      <App buildType={buildType} />
    </BrowserRouter>
  </Provider>
)

ReactDOM.render(app, document.getElementById('root'))

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
reportWebVitals();
