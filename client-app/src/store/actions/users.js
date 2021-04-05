import * as actionTypes from './actionTypes'
import axios from '../../axios'

export const fetchUsersStart = () => ({
    type: actionTypes.FETCH_USERS_START,
    loading: true,
    error: null
})

export const fetchUsersSuccess = (users) => ({
    type: actionTypes.FETCH_USERS_SUCCESS,
    loading: false,
    users,
    error: null
})

export const fetchUsersFail = (error) => ({
    type: actionTypes.FETCH_USERS_FAIL,
    users: [],
    loading: false,
    error
})


export const fetchUsers = () => {

    const url = '/users';

    return (dispatch) => {
        dispatch(fetchUsersStart())
        axios
            .get(url)
            .then((res) => {
                const fetchedUsers = []
                for (let key in res.data) {
                    fetchedUsers.push({
                        ...res.data[key],
                        id: key
                    })
                }
                dispatch(fetchUsersSuccess(fetchedUsers))
            })
            .catch((err) => {
                dispatch(fetchUsersFail(err))
            })
    }
}