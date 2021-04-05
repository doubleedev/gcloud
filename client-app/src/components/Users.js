import React from 'react'
import { useDispatch, useSelector } from 'react-redux'

import * as actions from '../store/actions'

const Users = () => {

    const dispatch = useDispatch()
    const users = useSelector((state) => state.users.users)

    const getUsers = () => {
        console.log('get users')
        dispatch(actions.fetchUsers())
    }

    return (
        <div>
            <h1>Users</h1>
            <button onClick={getUsers}>Fetch Users</button>
            <ul>
                {users.map(u => <li>{u.name}</li>)}
            </ul>
        </div>
    )
}

export default Users
