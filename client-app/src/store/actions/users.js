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

export const getSomethingStart = () => ({
    type: actionTypes.GET_SOMETHING_START,
    loading: true,
    error: null
})

export const getSomethingSuccess = something => ({
    type: actionTypes.GET_SOMETHING_SUCCESS,
    loading: false,
    something,
    error: null
})

export const getSomethingFail = (error) => ({
    type: actionTypes.GET_SOMETHING_FAIL,
    something: '',
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

export const getSomething = () => {

    const url = '/users/getsomething';

    return (dispatch) => {
        dispatch(getSomethingStart())
        axios
            .get(url)
            .then((res) => {
                dispatch(getSomethingSuccess(res.data))
            })
            .catch((err) => {
                dispatch(getSomethingFail(err))
            })
    }
}