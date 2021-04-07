import React from 'react'
import { useDispatch, useSelector } from 'react-redux'

import * as actions from '../store/actions'

const Home = () => {

    const dispatch = useDispatch()
    const something = useSelector((state) => state.users.something)

    const getUsers = () => {
        console.log('get Something')
        dispatch(actions.getSomething())
    }

    return (
        <div>
            <h1>Home</h1>
            <p>Welcome</p>

            <button onClick={getUsers}>Get something</button>
            <h2>This is what I got: {something}</h2>
        </div>
    )
}

export default Home
